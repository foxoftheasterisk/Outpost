
--TODO: make this a real function
--which involves doing some like pattern building beforehand
Game.buildChunk = function(chunk : Chunk, location : ChunkAddress)
	Game.LoadingScreen.change("Building chunk at " .. location.ToString())
	Log("Building chunk at " .. location)
	if location.position.Y == 0 then
		buildSurface(chunk,location, basics.dirt, basics.air)
	elseif location.position.Y < 0 then
		buildSolid(chunk, basics.dirt)
	elseif location.position.Y > 0 then
		buildSolid(chunk, basics.air)
	else
		Log("Chunk's Y is not a number??")
	end
end



--we'll be needing these
--a lot
const numPortions : int = 5
--if numPortions doesn't divide into chunkEdge evenly,
--odd things will happen at the high ends of the chunk

--so... I'm pretty sure if chunkSize is changed after this file is run, it will fail explosively
--so maybe don't do that
local chunkSize : int = Game.Sizes.ChunkSize
local chunkEdge : int = chunkSize - 1
local portionSize : int = math.floor(chunkEdge / numPortions)

--magic number!
const deltaSlopeMax = .2
const slopeStartMax = .1

local findBoundaryLines
local findLines
local findLine
local searchLines

function buildSolid(chunk : Chunk, fill : material)
	for x=0,chunkEdge do
		for y=0,chunkEdge do
			for z=0,chunkEdge do
				chunk.fillAssign(x,y,z,SolidBlock(fill))
			end
		end
	end
end

--I'm putting enough effort and durability into this it'll probably be a permanent function after all
--just not the only one
function buildSurface(chunk : Chunk, location : IntVector3, bottom : material, top : material)

	local boundaryLines = searchLines(chunk, location)

	--okay, so this algorithm doesn't take into account future goals
	--like, you know, the opposite end of the chunk if it's defined
	--that's something to add later

	--also it kind of assumes that there isn't definition on the opposite end
	--not entirely sure what'll happen if there is
	--it shouldn't explode too bad but it might be a bit weird

	local checkpoints = twoDArray(numPortions + 1) --I beLIEVE the +1 is necessary...
	local maxCount = 0
	local start = { }

	--this stuff is basically all data management
	foreach r in boundaryLines do
		local line = r.value
		
		--[[
		--make vector have +-1 for non-y value
		if line.vector.Y == 0 then
			goto continue
		elseif line.vector.X ~= 0 then
			line.vector = line.vector / math.abs(line.vector.X)
		elseif line.vector.Z ~= 0 then
			line.vector = line.vector / math.abs(line.vector.Z)
		else
			Log("ERROR: mutant non-planar boundary line!")
		end

		--move the base to ON the edge of the chunk rather than OFF it
		line.base = line.base - line.vector
		--]]

		--these should be integers
		--but just in case, floor
		local cX : int = math.floor(line.base.X / portionSize)
		local cZ : int = math.floor(line.base.Z / portionSize)

		--create the slot if needed
		if checkpoints[cX, cZ] == nil then
			checkpoints[cX, cZ] = { }
			checkpoints[cX, cZ].count = 0
		end

		--increase count
		checkpoints[cX, cZ].count = checkpoints[cX, cZ].count + 1
		if checkpoints[cX, cZ].count > maxCount then
			maxCount = checkpoints[cX, cZ].count
			start.X = cX
			start.Z = cZ
		end

		--store data
		table.insert(checkpoints[cX, cZ], line)
		::continue::
	end

	--not sure if this works properly
	--if it doesn't though should be a syntax error, right?
	local xStart, xEnd, xDir
	local zStart, zEnd, zDir

	--it is possible that, in the case that this chunk borders only one other chunk, (or borders only two opposing chunks)
	--the start position will be in the middle of an edge.
	--however, in this case the start position is unimportant so aligning it to a corner is perfectly fine.
	--(it would, on the other hand, be a problem if we used start.X and start.Z as the raw positions, as some checkpoints would be skipped.)
	if start.X == 0 then
		xStart = 0
		xEnd = numPortions
		xDir = 1
	else
		xStart = numPortions
		xEnd = 0
		xDir = -1
	end

	if start.Z == 0 then
		zStart = 0
		zEnd = numPortions
		zDir = 1
	else
		zStart = numPortions
		zEnd = 0
		zDir = -1
	end

	finishedCheckpoints = twoDArray(numPortions + 1)

	--doing this x-z may actually effect the output
	--...
	--no, actually, it shouldn't
	--but uh
	--i'm'a have to generate perpendicular lines
	--i guess starting with the second run? cause the first would be full arbitrary
	--then again, maybe that's good?
	--i suspect it depends on the area
	--let's go for it, I guess?
	for x=xStart,xEnd,xDir do
		for z=zStart,zEnd,zDir do

			--average of data points
			local avgY
			if checkpoints[x,z] ~= nil then
				local sumY : float = 0
				for i=1,checkpoints[x,z].count do
					local line = checkpoints[x,z][i]

					sumY = sumY + line.base.Y  --ah feck
				end

				avgY = sumY / checkpoints[x,z].count
			else
				avgY = math.random() * chunkEdge

				checkpoints[x, z] = { }
				checkpoints[x, z].count = 0
				--just to prevent more breakage
			end

			local greatestDiffY : float = 0
			--find how much this is already drifting from prescribed points
			--i guess we might want to have this signed?
			--but it doesn't matter 
			for i=1,checkpoints[x,z].count do
				local line = checkpoints[x,z][i]
				
				local diff : float = math.abs(line.base.Y - avgY)
				if diff > greatestDiffY then
					greatestDiffY = diff
				end
			end
			local randLimit = math.max(greatestDiffY - deltaSlopeMax, 0)
			local rand = math.random()*randLimit
			rand = rand - (randLimit / 2)
			local y = avgY + rand

			finishedCheckpoints[x,z] = { }
			finishedCheckpoints[x,z].Y = y

			for i=1,checkpoints[x,z].count do
				local line = checkpoints[x,z][i]

				local diff : float = float, line.base.Y - y
				local vector = line.vector
				vector.Y = vector.Y - 2 * diff
				local base = line.base + vector * 3

				local cX, cZ
				if vector.X ~= 0 then
					--guaranteeing a positive x/z for the vector (the x/z should already be guaranteed magnitude one)
					--this makes the next part's code easier
					if vector.X ~= 1 then
						vector = vector / vector.X
					end
					if finishedCheckpoints[x,z].X == nil then
						finishedCheckpoints[x,z].X = vector.Y
					else
						--uhhhh
						--handle this later whateves!
						--oh wait but
						goto continue
					end

					cX = x + xDir
					cZ = z
				elseif vector.Z ~= 0 then
					if vector.Z ~= 1 then
						vector = vector / vector.Z
					end
					if finishedCheckpoints[x,z].Z == nil then
						finishedCheckpoints[x,z].Z = vector.Y
					else
						--uhhhh
						--handle this later whateves!
						goto continue
					end

					cX = x
					cZ = z + zDir
				else
					Log("uhhhhhhhh how did this happen? this boundary line has no x or z factor?")
				end

				if cX < 0 or cX > numPortions or cZ < 0 or cZ > numPortions then
					goto continue
				end

				--create the slot
				if checkpoints[cX, cZ] == nil then
					checkpoints[cX, cZ] = { }
					checkpoints[cX, cZ].count = 0
				end

				--increase count
				checkpoints[cX, cZ].count = checkpoints[cX, cZ].count + 1

				--store data
				local newLine = { }
				newLine.base = base
				newLine.vector = vector
				table.insert(checkpoints[cX, cZ], newLine)

				::continue::
			end

			--create lines if they don't exist
			if finishedCheckpoints[x,z].X == nil then
				--pure random for now
				local slope = math.random() * (slopeStartMax * 2) - slopeStartMax
				finishedCheckpoints[x,z].X = slope

				cX = x + xDir
				cZ = z
				if cX < 0 or cX > numPortions then
					goto skip
				end
				
				--create the slot
				if checkpoints[cX, cZ] == nil then
					checkpoints[cX, cZ] = { }
					checkpoints[cX, cZ].count = 0
				end

				--increase count
				checkpoints[cX, cZ].count = checkpoints[cX, cZ].count + 1

				local vector = Vector3(1, slope, 0)
				local base = Vector3(x * portionSize, y, z * portionSize) + vector * portionSize

				--store data
				local newLine = { }
				newLine.base = base
				newLine.vector = vector
				table.insert(checkpoints[cX, cZ], newLine)

				::skip::
			end

			if finishedCheckpoints[x,z].Z == nil then
				--pure random for now
				local slope = math.random() * (slopeStartMax * 2) - slopeStartMax
				finishedCheckpoints[x,z].Z = slope

				cX = x
				cZ = z + zDir
				if cZ < 0 or cZ > numPortions then
					goto skip
				end

				--create the slot
				if checkpoints[cX, cZ] == nil then
					checkpoints[cX, cZ] = { }
					checkpoints[cX, cZ].count = 0
				end

				--increase count
				checkpoints[cX, cZ].count = checkpoints[cX, cZ].count + 1

				local vector = Vector3(0, slope, 1)
				local base = Vector3(x * portionSize, y, z * portionSize) + vector * portionSize

				--store data
				local newLine = { }
				newLine.base = base
				newLine.vector = vector
				table.insert(checkpoints[cX, cZ], newLine)

				::skip::
			end
		end
	end

	--okay, now we just fill the chunk based on those lines!
	for x=0,chunkEdge do
		for z=0,chunkEdge do

			--find the y at these coordinates
			local smallX = x / portionSize
			local smallZ = z / portionSize
			local xFraction = smallX - math.floor(smallX)
			local zFraction = smallZ - math.floor(smallZ)

			local cornerYs = { }
			do
				local corner = finishedCheckpoints[math.floor(smallX), math.floor(smallZ)]
				cornerYs["0,0"] = corner.Y + corner.X * xFraction * portionSize + corner.Z * zFraction * portionSize
				--I... think that's right...
			end
			do
				local corner = finishedCheckpoints[math.floor(smallX), math.ceil(smallZ)]
				cornerYs["0,1"] = corner.Y + corner.X * xFraction * portionSize + corner.Z * (zFraction - 1) * portionSize
			end
			do
				local corner = finishedCheckpoints[math.ceil(smallX), math.floor(smallZ)]
				cornerYs["1,0"] = corner.Y + corner.X * (xFraction - 1) * portionSize + corner.Z * zFraction * portionSize
			end
			do
				local corner = finishedCheckpoints[math.ceil(smallX), math.ceil(smallZ)]
				cornerYs["1,1"] = corner.Y + corner.X * (xFraction - 1) * portionSize + corner.Z * (zFraction - 1) * portionSize
			end


			local yTarget = cornerYs["0,0"] * (1 - xFraction) * (1 - zFraction) + 
							cornerYs["1,0"] * xFraction * (1 - zFraction) +
							cornerYs["0,1"] * (1 - xFraction) * zFraction +
							cornerYs["1,1"] * xFraction * zFraction


			--okay, now, it is just building up to that target!
			for y=0,chunkEdge do
				local block
				if y < yTarget then
					block = SolidBlock(bottom)
				else
					block = SolidBlock(top)
				end
				chunk.fillAssign(x,y,z,block);
			end

		end	
	end

	do
		local s = Structure()
		s.surface = finishedCheckpoints
		s.name = "surface"
		chunk.structures.Add(s)
	end
end

searchLines = function(chunk, location)
	
	--local top = Game.map.getPatternOrChunk(location + Game.Directions.up)
	--local bottom = Game.map.getPatternOrChunk(location + Game.Directions.down)
	local east = Game.map.getPatternOrChunk(location + Game.Directions.east)
	local west = Game.map.getPatternOrChunk(location + Game.Directions.west)
	local north = Game.map.getPatternOrChunk(location + Game.Directions.north)
	local south = Game.map.getPatternOrChunk(location + Game.Directions.south)

	local boundaryLines = { }

	local isSurface  = function(test : Structure) : bool
		if test.name == "surface" then
			return true
		else
			return false
		end
	end

	if east.chunk ~= nil then
		local surface = east.chunk.structures.Find(isSurface)
		if surface.name == "surface" then
			Log("successfully found a surface, east")

			local z = 0
			for x=0,numPortions do
				local point = surface.surface[x,z] --that's ridiculous
				local line = { }

				line.base = Vector3(x * portionSize, point.Y, chunkEdge + 1)
				line.vector = Vector3(0, point.Z, 1)
				line.base = line.base - line.vector  --it would probably be more efficient just to do point.Y - point.Z in the first place but this way makes it more clear what's happening

				table.insert(boundaryLines, line)
			end

		else
			Log("Found no surface at " .. location + Game.Directions.east)
			--I should probably actually handle this case...
		end
	end

	if west.chunk ~= nil then
		local surface = west.chunk.structures.Find(isSurface)
		if surface.name == "surface" then
			Log("successfully found a surface, west")

			local z = numPortions
			for x=0,numPortions do
				local point = surface.surface[x,z]
				local line = { }

				line.base = Vector3(x * portionSize, point.Y, -1)
				line.vector = Vector3(0, point.Z, 1)
				line.base = line.base + line.vector

				table.insert(boundaryLines, line)
			end

		else
			Log("Found no surface at " .. location + Game.Directions.west)
			--I should probably actually handle this case...
		end
	end

	if north.chunk ~= nil then
		local surface = north.chunk.structures.Find(isSurface)
		if surface.name == "surface" then
			Log("successfully found a surface, north")

			local x = 0
			for z=0,numPortions do
				local point = surface.surface[x,z]
				local line = { }

				line.base = Vector3(chunkEdge + 1, point.Y, z * portionSize)
				line.vector = Vector3(1, point.X, 0)
				line.base = line.base - line.vector

				table.insert(boundaryLines, line)
			end

		else
			Log("Found no surface at " .. location + Game.Directions.north)
			--I should probably actually handle this case...
		end
	end

	if south.chunk ~= nil then
		local surface = south.chunk.structures.Find(isSurface)
		if surface.name == "surface" then
			Log("successfully found a surface, south")

			local x = numPortions
			for z=0,numPortions do
				local point = surface.surface[x,z]
				local line = { }

				line.base = Vector3(-1, point.Y, z * portionSize)
				line.vector = Vector3(1, point.X, 0)
				line.base = line.base + line.vector

				table.insert(boundaryLines, line)
			end

		else
			Log("Found no surface at " .. location + Game.Directions.south)
			--I should probably actually handle this case...
		end
	end

	Log("done with surface finding")
	return boundaryLines
end

--]]

--finds all the lines for the chunk
--this may not actually make sense as a thing to do, please hold
findBoundaryLines = function(chunk, location)
	

	--neighbours
	local top = Game.map.getPatternOrChunk(location + Game.Directions.up)
	local bottom = Game.map.getPatternOrChunk(location + Game.Directions.down)
	local east = Game.map.getPatternOrChunk(location + Game.Directions.east)
	local west = Game.map.getPatternOrChunk(location + Game.Directions.west)
	local north = Game.map.getPatternOrChunk(location + Game.Directions.north)
	local south = Game.map.getPatternOrChunk(location + Game.Directions.south)

	--find boundary lines
	
	
	--[[
	--go clockwise around N,E,U
	--it may not make a difference but I believe being consistent will help the lines match up better
	if top.chunk ~= nil then
		--going east and south
		local base = Game.Directions.north * chunkEdge
		local offset = Game.Directions.up * chunkSize

		local result = findLines(top.chunk, base, Game.Directions.south, Game.Directions.east, Game.Directions.up)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
		
		local result = findLines(top.chunk, base, Game.Directions.east, Game.Directions.south, Game.Directions.up)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
	end

	if bottom.chunk ~= nil then
		--going west and north
		local base = Game.Directions.east * chunkEdge + Game.Directions.up * chunkEdge
		local offset = Game.Directions.down * chunkSize

		local result = findLines(bottom.chunk, base, Game.Directions.north, Game.Directions.west, Game.Directions.down)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
		
		local result = findLines(bottom.chunk, base, Game.Directions.west, Game.Directions.north, Game.Directions.down)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
	end
	--]]

	if east.chunk ~= nil then
		--going down and north
		local base = Game.Directions.up * chunkEdge
		local offset = Game.Directions.east * chunkSize

		local result = findLines(east.chunk, base, Game.Directions.down, Game.Directions.north, Game.Directions.east)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
		
		local result = findLines(east.chunk, base, Game.Directions.north, Game.Directions.down, Game.Directions.east)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
	end

	if west.chunk ~= nil then
		--going up and south
		local base = Game.Directions.north * chunkEdge + Game.Directions.east * chunkEdge
		local offset = Game.Directions.west * chunkSize

		local result = findLines(west.chunk, base, Game.Directions.up, Game.Directions.south, Game.Directions.west)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
		
		local result = findLines(west.chunk, base, Game.Directions.south, Game.Directions.up, Game.Directions.west)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
	end

	if north.chunk ~= nil then
		--going up and west
		local base = Game.Directions.east * chunkEdge
		local offset = Game.Directions.north * chunkSize

		local result = findLines(north.chunk, base, Game.Directions.up, Game.Directions.west, Game.Directions.north)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
		
		result = findLines(north.chunk, base, Game.Directions.west, Game.Directions.up, Game.Directions.north)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
		
	end

	if south.chunk ~= nil then
		--going down and east
		local base = Game.Directions.up * chunkEdge + Game.Directions.north * chunkEdge
		local offset = Game.Directions.south * chunkSize

		local result = findLines(south.chunk, base, Game.Directions.down, Game.Directions.east, Game.Directions.south)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
		
		local result = findLines(south.chunk, base, Game.Directions.east, Game.Directions.down, Game.Directions.south)
		foreach r in result do
			local line = r.value
			line.base = line.base + offset
			table.insert(boundaryLines, line)
		end
	end

	return boundaryLines
end



--finds the lines for one edge of the chunk
findLines = function(chunk, baseVector, dir1, dir2, inward)
	local lines = {}
	for i=0,numPortions do
		local lastTerrain = nil

		local newbase
		if i == 0 then
			newbase = baseVector
		else
			newbase = baseVector + (i * portionSize * dir1)
		end
		
		for j=0,15 do
			local block = newbase + (dir2 * j)
			local terrain = chunk.getBlock(block).primary
			if lastTerrain == nil then lastTerrain=terrain end

			if lastTerrain ~= terrain then
				local line = findLine(chunk, block, inward, dir2)
				if not line.bad then
					table.insert(lines, line)
				else
					--why is it getting one-long bits anyway?
				end
				lastTerrain = terrain
			end
		end
	end

	return lines
end


findLine = function(chunk, baseBlock, inward, slopeDir)

	--using weighted linear regression
	local n : float = 0
	local sumI = 0
	local sumS = 0
	local sumITimesS = 0
	local sumISquared = 0

	local terrain = chunk.getBlock(baseBlock).primary
	local borderTerrain = chunk.getBlock(baseBlock - slopeDir).primary
	local s = 0

	for i=0,9 do
		
		local testBlock = chunk.getBlock(baseBlock + inward*i + s*slopeDir)
		if testBlock == nil then break end

		if testBlock.primary == terrain then
			while testBlock.primary == terrain do
				s = s - 1
				testBlock = chunk.getBlock(baseBlock + inward*i + (s)*slopeDir)
				if testBlock == nil then break end
			end
			s = s + 1 --loop-and-a-half going on here
		elseif testBlock.primary == borderTerrain then
			while testBlock.primary == borderTerrain do
				s = s + 1
				testBlock = chunk.getBlock(baseBlock + inward*i + (s)*slopeDir)
				if testBlock == nil then break end
			end
		end

		if testBlock == nil then break end
		if testBlock.primary ~= terrain and testBlock.primary ~= borderTerrain then break end

		local weight = (10 - i)

		n = n + weight
		sumI = sumI + i * weight
		sumS = sumS + s * weight
		sumITimesS = sumITimesS + i * s * weight
		sumISquared = sumISquared + i * i * weight

		--there may be a better way to weight linear regression but this seems fine
		--though it might not work if we weren't guaranteed to go through the origin...
		--...wait
		--...does this actually work correctly
		--...no i think it does, it's not like we're actually moving the point logically; the multiplications are still based on the real position
	end

	--lua doesn't do integer division, so i don't need to worry about loss of precision
	--although actually NeoLua might... hmm.
	local slope = (sumITimesS - sumI * sumS / n) / (sumISquared - sumI * sumI / n)
	local intercept = sumS / n - slope * sumS / n
	
	--now to convert that to vector
	local line = {}
	line.base = baseBlock + slopeDir * intercept
	line.vector = inward + slopeDir * slope

	--stupid workaround
	if n == 10 then
		line.bad = true
	end

	line.t1 = terrain
	line.d1 = slopeDir * -1
	line.t2 = borderTerrain
	line.d2 = slopeDir
	--this... might be backwards
	--...but it's not like I'm using 'em anyway

	return line
end
